import React from 'react';

var footer = {
    fontSize: 12,
    textAlign: "center",
    left: "0",
    bottom: "0",
    height: "30px",
    width: "100%",
}

export function Footer() {
    return (
        <div style={footer}>
            Powered by <a href="https://github.com/kirnosenko/">Repositorch</a> 0.0.1 alpha
        </div>
    )
}
