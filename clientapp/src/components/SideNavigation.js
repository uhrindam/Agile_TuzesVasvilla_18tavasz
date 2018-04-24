import React from "react";

const SideNavigation = (props) => (
    <div className="sidenav">
        <img src="images/profile.jpg" className="img-rounded" />
        <div className="flex-center">
            <h4>Sipos Gergő</h4>
        </div>
        <div className="flex-center">
            <h3>${Math.round(props.usd * 10000) / 10000}</h3>
        </div>
        <div className="flex-center">
            <button onClick={props.resetBalance} type="button" className="btn btn-danger">Reset</button>
        </div>

        <div className="button-down">
            <button type="button" className="btn btn-danger">Log out</button>
        </div>
    </div>
)

export default SideNavigation;