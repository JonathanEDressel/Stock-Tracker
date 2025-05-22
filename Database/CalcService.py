from flask import Flask, jsonify, request, abort
import sqlite3

app = Flask(__name__)

MAX_VALUES = {
    "401k_MAX_CONTRIB_UNDER_65": 23000,
    "401k_CATCH_UP": 7500,
    "ROTH_IRA_MAX_CONTRIB_UNDER_50": 7000,
    "ROTH_IRA_MAX_CONTRIB_AFTER_50": 8000
}

def calc_stock_value(shares, currPrice):
    res = 0
    s = float(shares)
    p = float(currPrice)
    res = round(s * p, 2)
    return res

#Just a 401k with Employer match
def calc_401k_value(userInfo):
    return 0

#Include all retirement accounts
def calc_retirement_value():
    return 0

#Calculate savings value
def calc_saving_value():
    return 0

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5002)