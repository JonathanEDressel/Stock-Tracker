from flask import Flask, jsonify, request, abort
from flask_limiter import Limiter
from flask_limiter.util import get_remote_address
from dotenv import load_dotenv
import sqlite3
import PortfolioData
import requests
import os

load_dotenv()

api_key = os.getenv("API_KEY")
base_url = os.getenv('BASE_URL')

app = Flask(__name__)

limiter = Limiter(
    get_remote_address,
    app=app,
    default_limits=["20 per minute"]
)

def query_db(query, args=(), one=False):
    conn = sqlite3.connect('stocks.db')
    conn.row_factory = sqlite3.Row
    cur = conn.cursor()
    cur.execute(query, args)
    rv = cur.fetchall()
    conn.close()
    return (rv[0] if rv else None) if one else rv

def fetchStockData(symbol):
    infoUrl = f"{base_url}/stock/profile2?symbol={symbol}&token={api_key}"
    stockInfo = requests.get(infoUrl)
    stockInfo.raise_for_status()

    if stockInfo.status_code == 200:
        data = {**stockInfo.json()}
        if data.get("logo") is None:
            return {"error": "Could not find stock"}, 404 
        return {
            "logo_url": data.get("logo")
        } 

def fetchData(symbol):
    priceUrl = f"{base_url}/quote?symbol={symbol}&token={api_key}"
    priceRes = requests.get(priceUrl)
    priceRes.raise_for_status()

    profileUrl = f"{base_url}/stock/profile2?symbol={symbol}&token={api_key}"
    profileRes = requests.get(profileUrl)
    profileRes.raise_for_status()
    
    dbRes = PortfolioData.get_stock(symbol)
    if priceRes.status_code == 200 and profileRes.status_code == 200:
        if (dbRes is None or dbRes.get("company") is None): #check if stock exists in database
            data = {**priceRes.json(), **profileRes.json()}
            if data.get("name") is None:
                return {"error": "Could not find stock"}, 404 
            return {
                "id": 0,
                "symbol": symbol,
                "name": data.get("name"),
                "sharesOwned": 0,
                "sector": data.get("finnhubIndustry"),
                "current_price": data.get("c"),
                "open_price": data.get("o"),
                "high_price": data.get("h"),
                "low_price": data.get("l"),
                "logo_url": data.get("logo")
            }, 200
        else:
            data = {**priceRes.json(), **profileRes.json(), **dbRes}
            if data.get("name") is None:
                return {"error": "Could not find stock"}, 404

            return {
                "id": data.get("id"),
                "symbol": symbol,
                "name": data.get("name"),
                "sharesOwned": data.get("sharesOwned"),
                "sector": data.get("finnhubIndustry"),
                "current_price": data.get("c"),
                "open_price": data.get("o"),
                "high_price": data.get("h"),
                "low_price": data.get("l"),
                "logo_url": data.get("logo")
            }, 200
    else:
        return jsonify({"message": "Stock does not exist"}), 400

#API calls
@app.route('/api/stock/<symbol>', methods=['GET'])
@limiter.limit("100 per minute")
def getUserStockData(symbol):
    data = fetchData(symbol)
    return data

#Database calls
@app.route('/stocks/<int:id>', methods=['DELETE'])
@limiter.limit("20 per minute")
def delete_stock(id):
    return PortfolioData.delete_stock(id)

@app.route('/stocks/stock/', methods=['POST'])
@limiter.limit("20 per minute")
def addStockData():
    return PortfolioData.add_stock()

@app.route('/stocks/stock/<int:id>', methods=['PATCH'])
@limiter.limit("20 per minute")
def editSharesHeld(id):
    shares = request.json.get('sharesOwned')
    return PortfolioData.edit_stock_shares(id, shares)

@app.route('/stocks', methods=['GET'])
@limiter.limit("20 per minute")
def getUserStocks():
    dbData = PortfolioData.get_stocks()
    res = []
    for d in dbData:
        tmp = fetchStockData(d['symbol'])
        res.append({**tmp, **d})
    if res:
        return res
    else:
        return jsonify({"error:", "Could not get stocks"}), 404

@app.route('/')
@limiter.limit("10 per hour")
def index():
    return "running stocker"

if __name__ == '__main__':
    #limiter.enabled = False
    app.run(host='0.0.0.0', port=5000)