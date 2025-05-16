from flask import Flask, jsonify, request, abort
import sqlite3

app = Flask(__name__)

def calc_stock_value(shares, currPrice):
    res = 0
    print('shares - ', shares)
    s = float(shares)
    p = float(currPrice)
    res = round(shares * currPrice, 2)
    print('res - ', res)
    return res;

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5002)