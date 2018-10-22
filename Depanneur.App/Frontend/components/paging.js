import React from "react";
import Radium from "radium";
import MediaQuery from "react-responsive";

const Paging = props => {
    return (
        <MediaQuery minWidth={800}>
            {(matches) => {
                return matches
                    ? <DesktopPaging {...props} />
                    : <MobilePaging {...props} />
            }}
        </MediaQuery>
    );
}

const DesktopPaging = Radium(props => {
    const PAGE_SPAN = 5;
    const pages = [];

    if (props.current > 1) {
        pages.push(<span key="prev" style={[styles.page, styles.first, styles.clickable]} onClick={() => props.onPageChanged(props.current - 1)}>«</span>)
    } else {
        pages.push(<span key="prev" style={[styles.page, styles.first, styles.disabled]}>«</span>)
    }

    for (let i = 1; i <= props.count; i++) {
        if (i === props.current) {
            pages.push(<span key={i} style={[styles.page, styles.current]}>{i}</span>);
        } else {

            if (i > 1 && i < props.current - PAGE_SPAN) {
                pages.push(<span style={[styles.page, styles.disabled]} key={i}>...</span>);
                i = props.current - PAGE_SPAN;
            } else if (i < props.count && i > props.current + PAGE_SPAN) {
                pages.push(<span style={[styles.page, styles.disabled]} key={i}>...</span>);
                i = props.count;
            }

            pages.push(<span key={i} style={[styles.page, styles.clickable]} onClick={() => props.onPageChanged(i)}>{i}</span>);
        }
    }

    if (props.current < props.count) {
        pages.push(<span key="next" style={[styles.page, styles.last, styles.clickable]} onClick={() => props.onPageChanged(props.current + 1)}>»</span>)
    } else {
        pages.push(<span key="next" style={[styles.page, styles.last, styles.disabled]}>»</span>)
    }

    return (
        <div style={props.style}>{pages}</div>
    );

});

const MobilePaging = Radium(props => {

    let previousButton, nextButton;

    if (props.current > 1)
        previousButton = <span key="previous" style={[styles.page, styles.standalone, styles.clickable]} onClick={() => props.onPageChanged(props.current - 1)}>Précédent</span>;
    else
        previousButton = <span key="previous" style={[styles.page, styles.standalone, styles.disabled]}>Précédent</span>;

    if (props.current < props.count)
        nextButton = <span key="next" style={[styles.page, styles.standalone, styles.clickable]} onClick={() => props.onPageChanged(props.current + 1)}>Suivant</span>;
    else
        nextButton = <span key="next" style={[styles.page, styles.standalone, styles.disabled]}>Suivant</span>;

    return (
        <div style={[props.style, styles.mobile]}>
            {previousButton}
            <div style={styles.mobileIndicator}>{props.current} / {props.count}</div>
            {nextButton}
        </div>
    );
});

const styles = {
    page: {
        cursor: "default",
        display: "inline-block",
        paddingLeft: 12,
        paddingRight: 12,
        border: "solid 1px #ccc",
        marginRight: -1,
        height: 36,
        lineHeight: "36px",
        
        ":hover": {
            // Doit rester même si vide: contourne un problème avec radium
        }
    },
    standalone: {
        borderRadius: 18
    },
    mobile: {
        display: "flex",
        justifyContent: "space-between"
    },
    mobileIndicator: {
        height: 36,
        lineHeight: "36px",
    },
    first: {
        borderTopLeftRadius: 5,
        borderBottomLeftRadius: 5
    },
    last: {
        borderTopRightRadius: 5,
        borderBottomRightRadius: 5
    },
    current: {
        backgroundColor: "#eee",
        fontWeight: "bold"
    },
    clickable: {
        cursor: "pointer",

        ":hover": {
            backgroundColor: "#eee"
        }
    },
    disabled: {
        color: "#ccc"
    }
}

export default Paging;