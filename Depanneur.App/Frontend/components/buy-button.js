import React from "react";
import Radium from "radium";

import CircleButton from "./circle-button";

class BuyButton extends React.Component {
    constructor() {
        super();

        this.state = {
            showConfirmation: false
        }
    }

    render() {

        if (this.state.loading) {
            return <Loader />;
        }
        else if (this.state.showConfirmation) {
            return <CheckMark />;
        }

        return <CircleButton onClick={this.onClick}>+1</CircleButton>;

    }

    componentWillUnmount() {
        if (this.timeout) {
            clearTimeout(this.timeout);
        }
    }

    onClick = () => {
        const result = this.props.onClick();

        if (result && result.then) {
            this.setState({
                loading: true
            });

            result
                .then(() => this.showConfirmation())
                .catch(() => this.setState({loading: false, showConfirmation: false}));
        }
        else {
            this.showConfirmation();
        }
    }

    showConfirmation() {
        this.setState({loading: false, showConfirmation: true});
        this.timeout = setTimeout(() => {
            this.setState({showConfirmation: false});
            this.timeout = null;
        }, 300);
    }
}

const Loader = Radium(() => {
    return <div style={[style.indicator, style.loader]}>⏳</div>
});

const CheckMark = Radium(() => {
    return <div style={[style.indicator, style.checkmark]}>✓</div>
});

const style = {
    indicator: {
        display: "inline-block",
        fontWeight: "bold",
        fontSize: 24,
        color: "#fff",
        textAlign: "center",
        width: 36,
        height: 36,
        lineHeight: "36px"
    },
    loader: {
        backgroundColor: "#EA0"
    },
    checkmark: {
        backgroundColor: "#090"
    }
}

export default BuyButton;