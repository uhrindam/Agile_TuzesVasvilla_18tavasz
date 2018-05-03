import React from "react";
import { currencies } from "../Constants";

const Account = (props) => {
    const balance = props.balance;
    const rates = props.rates;
    let sumValue = 0;
    sumValue += balance['usd'];
    return (
        <div>
            <table>
                <thead>
                    <tr>
                        <td>Currency</td>
                        <td>Amount</td>
                    </tr>
                </thead>
                <tbody>
                    {
                        Object.keys(balance).map((x) => {
                            if (x != "usd" && x != "token") {
                                const rate = rates == undefined ? undefined : rates.find((y) => {
                                    return (y.symbol == x.toUpperCase())
                                });
                                console.log(rate);
                                if (rate) {
                                    sumValue += balance[x] * rate.currentRate;
                                }
                                return (
                                    <tr key={x}>
                                        <td>{currencies[x]}</td>
                                        <td>{Math.round(balance[x] * 10000) / 10000}</td>
                                    </tr>
                                )
                            }
                        })
                    }
                </tbody>
            </table>
            <h5>Sum value: {Math.round(sumValue*10000) / 10000 }$ </h5>
        </div>
    )
}

export default Account;