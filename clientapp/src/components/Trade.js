import React, { Component } from "react";
import { currencies } from "../Constants";
import { transaction } from "../Services";
import { NotificationContainer, NotificationManager } from 'react-notifications';


class Trade extends React.Component {

    constructor(props) {
        super(props)
        //this.onBuy = this.onBuy.bind(this);
    }

    state = {
        amount: 0,
        symbol: Object.keys(currencies)[0].toUpperCase()
    }

    onAmountChange = (e) => {
        this.setState({ amount: parseFloat(e.target.value) });
    }

    onSymbolChange = (e) => {
        this.setState({ symbol: e.target.value.toUpperCase() });
    }

    onTransaction = (e) => {
        e.preventDefault();
        transaction({ Amount: this.state.amount, Symbol: this.state.symbol }, e.target.id)
            .then(() => { this.props.refreshBalance(); NotificationManager.success("Transaction succeded", 'Success'); })
            .catch(error => 
                { NotificationManager.error( error ? JSON.parse(error).Message : "Api endpoint is unreachable", 'Error') });
    }



    render() {
        return (
            <div className="content row">
                <div className="form-group col-md-12">
                    <label htmlFor="currency">Currency: </label>
                    <select value={this.state.value} className="form-control" id="currency" onChange={this.onSymbolChange}>
                        {
                            Object.keys(currencies).map((x) => (<option key={x} value={x}>{currencies[x]}</option>))
                        }
                    </select>
                </div>

                <div className="form-group  col-md-12">
                    <label htmlFor="amount">Amount: </label>
                    <input className="form-control" id="amount" type="number" onChange={this.onAmountChange} />
                </div>

                <div className="flex-center">
                    <div>
                        <button id="buy" onClick={this.onTransaction} type="button" className="btn btn-success">Buy</button>
                    </div>
                    <div>
                        <button id="sell" type="button" onClick={this.onTransaction} className="btn btn-danger">Sell</button>
                    </div>
                </div>

            </div>
        )
    }
}

export default Trade;