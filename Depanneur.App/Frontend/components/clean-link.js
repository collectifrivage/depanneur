import React from "react";
import Radium from "radium";
import {Link} from "react-router-dom";

const RadiumLink = Radium(Link);

const style = {
    cursor: "pointer",
    textDecoration: "none"
};

const CleanLink = props => {
    return <RadiumLink {...props} style={[style, props.style]} />
};

export default Radium(CleanLink);