import React from "react";
import Radium from "radium";
import commonStyles from "../styles";
import events from "../events";

class Notifications extends React.Component {
    constructor() {
        super();
        this.state = {
            visible: false
        }
    }

    componentWillMount() {
        events.on("show-notification", this.onNotification);
    }
    componentWillUnmount() {
        events.off("show-notification", this.onNotification);

        if (this.timeout) {
            clearTimeout(this.timeout);
        }
    }

    render() {
        if (!this.state.visible) return null;

        return (
            <div style={styles.notification}>
                <div style={commonStyles.block}>
                    {this.state.message}
                    {this.renderActions()}
                </div>
            </div>
        );
    }

    renderActions() {
        if (!this.state.actions || this.state.actions.length === 0) return null;

        return (
            <div style={styles.actionBar}>
                {this.state.actions.map((a, i) => <span key={i} style={styles.action} onClick={() => this.handleClick(a)}>{a.label}</span>)}
            </div>
        );
    }

    handleClick = action => {
        this.setState({visible: false});

        if (action.onClick) {
            action.onClick();
        }
    }

    onNotification = details => {
        if (this.timeout) {
            clearTimeout(this.timeout);
        }

        this.setState({
            visible: true,
            message: details.message,
            actions: details.actions
        });

        this.timeout = setTimeout(() => {
            this.setState({visible: false});
            this.timeout = null;
        }, details.timeout || 10000)
    }

}

const styles = {
    notification: {
        position: "fixed",
        bottom: 0,
        width: "100%",
        padding: "30px 0",

        backgroundColor: "#333",
        color: "#fff"
    },
    actionBar: {
        marginTop: 10
    },
    action: {
        textTransform: "uppercase",
        marginRight: 10,
        padding: "5px 0",
        fontWeight: "bold",
        color: "#0095dd",
        cursor: "pointer"
    }
};

export default Notifications;