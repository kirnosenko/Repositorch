import React from 'react';
import './Loading.css';

var loading = {
    alignItems: "center",
    textAlign: "center",
    width: "100%"
}

export default function Loading() {
    return (
        <div style={loading}>
            <div className="ldio-spinner">
                <div className="ldio-loading">
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
