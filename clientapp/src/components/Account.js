import React from "react";
import { currencies } from "../Constants";

const Account = (props) => {
    const balance = props.balance;
    return (
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
    )
}

export default Account;