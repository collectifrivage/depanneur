import React from "react";
import Radium from "radium";
import {Link} from "react-router-dom";
import Amount from "./amount";
import {Table, HeaderCell, Row, Cell} from "./tables";

const BalancesList = props => {
    let i = 0;
    const header = [
        <HeaderCell key={i++}>Nom</HeaderCell>,
        <HeaderCell key={i++} style={styles.right}>Solde</HeaderCell>,
        <HeaderCell key={i++} style={{width:200}}></HeaderCell>
    ];

    return (
        <Table header={header}>
            {props.balances.map(b => <BalanceRow key={b.id} data={b} />)}
        </Table>
    );
}

const BalanceRow = props => {
    return (
        <Row>
            <Cell style={props.data.isDeleted ? styles.deleted : null}>{props.data.name}</Cell>
            <Cell style={styles.right}><Amount amount={props.data.balance} /></Cell>
            <Cell style={styles.right}>
                <Link to={`/payment/${props.data.id}`}>Enregistrer un paiement</Link><br/>
                <Link to={`/adjustment/${props.data.id}`}>Ajustement de solde</Link><br/>
                <Link to={`/transactions/${props.data.id}`}>Voir transactions</Link>
            </Cell>
        </Row>
    )
};

const styles = {
    right: {
        textAlign: "right"
    },
    deleted: {
        color: "#aaa"
    }
}

export default BalancesList;