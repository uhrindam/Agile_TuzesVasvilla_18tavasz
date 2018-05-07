import React from "react";
import { currencies } from '../Constants'
import { fetchHistory } from "../Services";
import moment from "moment";

const itemsPerPage = 5;

class History extends React.Component {

    state = {
        history: [],
        pageIdx: 0,
        numOfEntries: 0
    }

    componentDidMount() {
        this.getBalance();
    }

    getBalance = () => {
        fetchHistory().then(
            history => { this.setState({ history: history.reverse(), numOfEntries: history.length }) })
            .catch(error => { NotificationManager.error(error, 'Error'); });
    };

    next = () => {
        if (this.state.pageIdx + itemsPerPage < this.state.numOfEntries) {
            this.setState((prevState) => ({ pageIdx: prevState.pageIdx + itemsPerPage }))
        }
    }

    previous = () => {
        if (this.state.pageIdx > 0) {
            this.setState((prevState) => ({ pageIdx: prevState.pageIdx - itemsPerPage }))
        }
    }

    render() {
        return (
            <div>
                <table className="table-common">
                    <thead>
                        <tr>
                            <td>#</td>
                            <td>Currency</td>
                            <td>Amount</td>
                            <td>Type</td>
                            <td>Time</td>
                        </tr>
                    </thead>
                    <tbody>
                        {
                            this.state.history.slice(this.state.pageIdx, this.state.pageIdx + itemsPerPage).map((h, index) =>
                                (<tr key={h.createdAt}>
                                    <td>{index + 1+this.state.pageIdx}</td>
                                    <td>{!h.symbol ? "-" : currencies[h.symbol.toLowerCase()]}</td>
                                    <td>{h.amount == 0 ? "-" : Math.round(h.amount*10000)/10000 }  </td>
                                    <td>{h.type}</td>
                                    <td>{moment(h.createdAt).format('YYYY-MM-DD, hh:mm:ss')}</td>
                                </tr>
                                ))
                        }
                    </tbody>
                </table>
                <hr />
                {(this.state.pageIdx / itemsPerPage)+1} / { Math.ceil((this.state.numOfEntries / itemsPerPage))}
                <div className="flex-center">
                    <div>
                        <button id="Previous" onClick={this.previous} type="button" className="btn btn-info">Previous</button>
                    </div>
                    <div>
                        <button id="Next" onClick={this.next} type="button" className="btn btn-info">Next</button>
                    </div>
                </div>
            </div>
        );
    }
}


export default History;