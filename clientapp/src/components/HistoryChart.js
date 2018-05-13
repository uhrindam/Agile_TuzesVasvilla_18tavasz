import React from "react";
import { currencies } from '../Constants'
import { Line } from 'react-chartjs-2';
import moment from "moment";

function randomColor() {
    return '#' + (0x1000000 + (Math.random()) * 0xffffff).toString(16).substr(1, 6);
}

class HistoryChart extends React.Component {


    render() {
        const data = { labels: [], datasets: [] };

        const history = this.props.history;
        const balanceData = [];

        if (history){
            for (let hist of history) {
                let sum = 0;
                const keys =  Object.keys(hist.exchangeRates);
                for (let k of keys) {
                    sum += hist.exchangeRates[k] * hist.balance[k];
                }
                sum += hist.balance["usd"];
                balanceData.push(sum);
                data.labels.push(moment(hist.createdAt).format("MM.DD, hh:mm") );
            }
        }

        data.labels = data.labels.slice(0,40).reverse();
        data.datasets.push({ label: "USD",data: balanceData.slice(0,40).reverse() });
        return (
            <div>
                <Line data={data} />
            </div>
        )

    }
}


export default HistoryChart;