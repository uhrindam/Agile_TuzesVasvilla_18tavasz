import React from "react";
import { Line } from 'react-chartjs-2';

function randomColor() {
    return '#' + (0x1000000 + (Math.random()) * 0xffffff).toString(16).substr(1, 6);
}

const numOfData = 50;

export function PrepareDiagramData(rates, data, numOfData) {
    rates.map((rate) => {
        var firstN_Element = rate.history ?  Object.values(rate.history).slice(0, numOfData).reverse():[];
        data.datasets.push({ label: rate.symbol, data: firstN_Element });
    })
}

class Chart extends React.Component {

    shouldComponentUpdate() { return false }

    render() {
        const data = { labels: [], datasets: [] };

        PrepareDiagramData(this.props.rates, data,numOfData);

        if (this.props.rates && this.props.rates[0])
            data.labels = Object.keys(this.props.rates[0].history).slice(0, numOfData).reverse();

        for (let dataset of data.datasets) {
            var color = randomColor()
            dataset.borderColor = color
            dataset.pointBorderColor = color
            dataset.pointBackgroundColor = color
            dataset.pointBorderWidth = 1
        }


        return (
            <div>
                <Line data={data} />
            </div>
        )

    }
}
export default Chart;