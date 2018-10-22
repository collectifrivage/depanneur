import React from "react";

const DateDisplay = props => {
    let d = props.date;
    if (!(d instanceof Date))
        d = new Date(d);

    var formattedDate = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
    var formattedHour = `${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;

    return (
        <span>
            <span style={{whiteSpace:"nowrap"}}>{formattedDate}</span> <span style={{whiteSpace:"nowrap"}}>{formattedHour}</span>
        </span>
    );
}

function pad(val) {
    if (val < 10) return "0" + val;
    return val.toString();
}

export default DateDisplay;