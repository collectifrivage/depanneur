import React from "react";

import { describeTransaction } from "../helpers/transaction-helper";
import Amount from "./amount";
import Date from "./date";
import { SubscriptionIcon } from "./icons";
import LinkButton from "./link-button";
import { Cell, HeaderCell, Row, Table } from "./tables";

const TransactionsList = props => {

    let i = 0;
    const header = [
        <HeaderCell key={i++}>Date et heure</HeaderCell>,
        <HeaderCell key={i++}>Description</HeaderCell>,
        <HeaderCell key={i++} style={styles.right}>Montant</HeaderCell>,
        <HeaderCell key={i++} style={styles.right}>Solde</HeaderCell>,
        <HeaderCell key={i++} />
    ];

    return (
        <Table header={header}>
            {props.transactions.map(t => <TransactionRow key={t.id} transaction={t} onDelete={() => props.onDelete(t)} />)}
        </Table>
    );

}

const TransactionRow = props => {

    const t = props.transaction;

    return (
        <Row>
            <Cell><Date date={t.timestamp} /></Cell>
            <Cell><TransactionDescription transaction={t} /></Cell>
            <Cell style={styles.right}><Amount amount={t.amount} /></Cell>
            <Cell style={styles.right}><Amount amount={t.newBalance} /></Cell>
            <Cell style={styles.right}>
                {(t.canDelete && props.onDelete) ? <LinkButton onClick={() => props.onDelete()}>Annuler</LinkButton> : null}
            </Cell>
        </Row>
    );

}

const TransactionDescription = ({transaction}) => {
    const description = describeTransaction(transaction);
    
    let icon = null;
    if (transaction.wasFromSubscription) icon = <SubscriptionIcon style={{verticalAlign: "middle", marginLeft: 5}} />;

    return (
        <React.Fragment>
            {description}
            {icon}
        </React.Fragment>
    );
}

const styles = {
    right: {
        textAlign: "right"
    }
}

export default TransactionsList;