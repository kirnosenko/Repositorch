import React from 'react';

var loading = {
    textAlign: "center",
    width: "100%"
}

function Loading() {
    return (
        <div style={loading}>
            <p><em>Loading...</em></p>
        </div>
    )
}

export default Loading