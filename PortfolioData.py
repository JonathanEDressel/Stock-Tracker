from flask import Flask, jsonify, request, abort
import sqlite3

app = Flask(__name__)

def query_db(query, args=(), one=False, commit=True):
    conn = sqlite3.connect('stocks.db')
    conn.row_factory = sqlite3.Row
    cur = conn.cursor()
    try:
        cur.execute(query, args)
        if commit:
            conn.commit()
        rv = cur.fetchall()
        return (rv[0] if rv else None) if one else rv
    finally:
        conn.close()

def get_stocks():
    res = query_db('SELECT * FROM stock_data')
    stocks = [dict(row) for row in res]
    return stocks

def get_stock(symbol):
    stocks = query_db('SELECT * FROM stock_data WHERE symbol = ?', [symbol.upper()])
    stock = [dict(row) for row in stocks]
    if (stock):
        return stock[0]
    else:
        return None

def edit_stock_shares(id, shares):
    try:
        id = int(id)
        shares = float(shares)
    except ValueError:
        return jsonify({"message": "Invalid ID format"}), 400
    with sqlite3.connect('stocks.db') as conn:
        cur = conn.cursor()
        cur.execute('UPDATE stock_data SET SharesOwned = ? WHERE id = ?', (shares, id))
        if cur.rowcount == 0:
            return jsonify({"message": "Record not found"}), 404
    return jsonify({"message": f"Record with ID {id} updated successfully"}), 200

def delete_stock(id):
    try:
        id = int(id)
    except ValueError:
        return jsonify({"message": "Invalid ID format"}), 400
    
    stocks = query_db('SELECT * FROM stock_data WHERE id = ?', [id])
    if not stocks:
        return jsonify({"message": "Record not found"}), 404

    query_db("DELETE FROM stock_data WHERE id = ?", [id])
    return jsonify({"message": f"Record with ID {id} deleted successfully"}), 200

def add_stock():
    if not request.json or 'Symbol' not in request.json:
        abort(400, "Invalid data: 'Symbol' is a required field.")

    symbol = request.json['Symbol']
    sharesOwned = request.json.get('SharesOwned')
    company = request.json.get('Company')
    sector = request.json.get('Sector')

    conn = sqlite3.connect('stocks.db')
    cur = conn.cursor()
    
    cur.execute("SELECT * FROM stock_data WHERE symbol = ?", (symbol,))
    record = cur.fetchone()
    
    if record:
        return jsonify({"message": "Record already exsis in database"}), 400

    cur.execute('''
        INSERT INTO stock_data (Symbol, SharesOwned, Company, Sector)
        VALUES (?, ?, ?, ?)
    ''', (symbol, sharesOwned, company, sector))
    conn.commit()
    new_id = cur.lastrowid
    conn.close()

    return jsonify({'id': new_id, 'company': company, 'symbol': symbol, 'sharesOwned': sharesOwned, 'sector': sector}), 200

if __name__ == '__main__':
    #app.run(debug=False)
    app.run(host='0.0.0.0', port=5001)