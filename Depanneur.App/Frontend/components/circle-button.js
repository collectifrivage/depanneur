import React from "react";
import Radium from "radium";
import Color from "color";

const CircleButton = (props) => {
    const {color, ...rest} = props;

    return (
        <button style={makeStyle(color)} {...rest} />
    );
}

function makeStyle(color) {
    if (!color) color = "#EEE";

    color = Color(color);
    
    const border = "3px solid " + color.darken(0.075).hex();

    return {
        cursor: "pointer",
        borderRadius: "50%",
        width: 36,
        height: 36,
        backgroundColor: color.hex(),

        borderTop: "none",
        borderLeft: "none",
        borderRight: "none",
        borderBottom: border,

        display: "flex",
        justifyContent: "space-around",

        ":hover": {
            backgroundColor: color.lighten(0.025).hex()
        },
        ":active": {
            borderBottom: "none",
            borderTop: border
        },
        ":focus": {
            outline: "none"
        }
    }
}

export default Radium(CircleButton);