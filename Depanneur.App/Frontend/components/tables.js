import React from "react";
import Radium from "radium";

const Table = props => {
    return (
        <table style={styles.table}>
            <thead>
                <tr style={styles.headerRow}>
                    {props.header}
                </tr>
            </thead>
            <tbody>
                {props.children}
            </tbody>
        </table>
    );
};

const HeaderCell = Radium(props => {
    return <td style={[styles.headerCell, props.style]}>{props.children}</td>;
});

const Row = Radium(props => {
    return <tr style={[styles.row, props.style]}>{props.children}</tr>;
});

const Cell = Radium(props => {
    return <td style={[styles.cell, props.style]}>{props.children}</td>;
});


const styles = {
    table: {
        width:"100%", 
        borderCollapse: "collapse"
    },
    headerRow: {
        backgroundColor: "#333"
    },
    headerCell: {
        color: "#FFF", 
        fontWeight:"bold", 
        textAlign: "left", 
        padding: 5
    },
    row: {

    },
    cell: {
        padding: "10px 5px",
        borderBottom: "solid 1px #CCC"
    }
};

export {
    Table,
    HeaderCell,
    Row,
    Cell
};