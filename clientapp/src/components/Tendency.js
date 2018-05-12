import React from "react";
import Ionicon from "react-ionicons";
import { currencies } from ".././Constants";

const Tendency = (props) => (<table className="table-common">
    <thead>
        <tr>
            <td>Currency</td>
            <td>Rate</td>
            <td>Tendency</td>
        </tr>
    </thead>
    <tbody>
        {
            props.rates.map(x => {
                var values = x.history ? Object.values(x.history) : [];
                return (
                    <tr key={x.symbol}>
                        <td>{currencies[x.symbol.toLowerCase()]}</td>
                        <td>{Math.round(x.currentRate * 100) / 100} USD</td>
                        <td>
                            {(values[0]) == values[1] ? <Ionicon icon="md-arrow-forward" fontSize="35px" color="orange" />
                                : (values[0]) > values[1] ? <Ionicon icon="md-trending-up" fontSize="35px" color="red" /> :
                                    <Ionicon icon="md-trending-down" fontSize="35px" color="green" />}
                        </td>
                    </tr>)
            })
        }
    </tbody>
</table>
)

export default Tendency;