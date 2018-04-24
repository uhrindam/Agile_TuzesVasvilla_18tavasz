import React from "react";

const withCard = WrappedComponent => class extends React.Component {
    render() {
        return (
            <div className="col-md">
                <div className="card card-margin">
                    <div className="card-header">
                        {this.props.title}
                    </div>
                    <div className="card-body">
                        <WrappedComponent {...this.props}/>
                    </div>
                </div>
            </div>
        )
    }
}

export default withCard;