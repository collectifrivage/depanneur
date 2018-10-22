import React from "react";

class LinkButton extends React.Component {
    render() {
        return (
            <span style={styles} {...this.props} />
        );
    }
}

const styles = {
    textDecoration: "underline",
    cursor: "pointer"
};

export default LinkButton;