import React from "react";

const ErrorMessage = props => {
    return <p style={style} {...props} />
};

const style = {
    color: "#f00"
}

export default ErrorMessage;