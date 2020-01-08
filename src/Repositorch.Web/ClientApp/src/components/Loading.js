import React from 'react';
import './Loading.css';

var loading = {
    alignItems: "center",
    textAlign: "center",
    width: "100%"
}

function Loading() {
    return (
        <div style={loading}>
            <div class="ldio-spinner">
                <div class="ldio-loading">
                    <div></div>
                    <div></div>
                    <div></div>
                    <div></div>
                    <div></div>
                    <div></div>
                    <div></div>
                    <div></div>
                    <div></div>
                    <div></div>
                </div>
            </div>
        </div>
    )
}

export default Loading