import React, { Component } from "react";

import SideNavigation from "./SideNavigation";

import Trade from "./Trade";
import Account from "./Account";
import Tendency from "./Tendency";
import Chart from "./Chart";
import HistoryComponent from "./History";
import HistoryChartComponent from "./HistoryChart";


import WithCard from "./Card";
import { fetchBalance, reset, getExchangeRate } from "../Services";
import { fetchHistory } from "../Services";
import { NotificationContainer, NotificationManager } from 'react-notifications';
import { currencies } from "../Constants";



const apiTimeOut = 100;



const initState = {
    balance: { usd: 0, xrp: 0, eth: 0, btc: 0 },
    rates: [],
    history: [],
    numOfEntries: 0
}

class CryptoTraderApp extends React.Component {

    state = initState;

    getBalance = () => {
        fetchBalance().then(
            balance => { this.setState({ balance }) })
            .catch(error => { NotificationManager.error(error, 'Error'); });
    };

    getHistory = () => {
        fetchHistory().then(
            history => { this.setState({ history: history.reverse(), numOfEntries: history.length }) })
            .catch(error => { NotificationManager.error(error, 'Error'); });
    };

    updateExchangeRate() {
        Object.keys(currencies)
            .map((x) => setTimeout(() => getExchangeRate(x.toLocaleLowerCase())
                .then(response => this.setState({ ...this.state, rates: [...this.state.rates, response] })), apiTimeOut)
            )
    }

    componentDidMount() {
        setTimeout(() => this.getBalance(), apiTimeOut)
        setTimeout(() => this.getHistory(), apiTimeOut)
        this.updateExchangeRate()
        setInterval(() => {
            this.updateExchangeRate()
            this.setState({ ...this.state, rates: [] })
        }, 1000 * 60 * 5);
    }

    resetBalance = () => {
        reset().then(() => {
            this.getBalance();
            NotificationManager.success('Balance reseted to 5000 USD.', 'Success');
        }).catch(error => { NotificationManager.error(error, 'Error'); })
    }

    render() {
        const TradeCard = (WithCard)(Trade);
        const AccountCard = (WithCard)(Account);
        const TendencyCard = (WithCard)(Tendency);
        const ChartCard = (WithCard)(Chart);
        const HistoryCard = (WithCard)(HistoryComponent)
        const HistoryChartCard = (WithCard)(HistoryChartComponent);

        return (
            <div>
                <NotificationContainer />
                <div className="container-fluid">
                    <SideNavigation usd={this.state.balance.usd} resetBalance={this.resetBalance} />
                    <div className="main">
                        <div className="row">
                            <AccountCard title="My Account" balance={this.state.balance} rates={this.state.rates} />
                            <TradeCard title="Trade" refreshBalance={this.getBalance} />
                        </div>
                        <div className="row">
                            <TendencyCard title="Tendency" rates={this.state.rates} />
                            <HistoryCard title="My History" history={this.state.history}
                                numOfEntries={this.state.numOfEntries} />
                        </div>
                        <div className="row">
                            <HistoryChartCard title="History chart" history={this.state.history} />
                            <ChartCard title="Exchange Rate" rates={this.state.rates} />
                        </div>
                    </div>
                </div>
            </div>
        )
    }
}

export default CryptoTraderApp;