import React from 'react';
import Metric from './Metric';

function renderActivity(data) {
    return (
        <table className='table table-striped'>
            <thead>
                <tr>
                    <th>Period</th>
                    <th>Commits (total)</th>
                    <th>Authors (total)</th>
                    <th>Files</th>
                    <th>Added LOC (total)</th>
                    <th>Removed LOC (total)</th>
                    <th>LOC</th>
                </tr>
            </thead>
            <tbody>
                {data.periods.map(period =>
                    <tr key={period.name}>
                        <td>{period.title}</td>
                        <td>{period.commits}</td>
                        <td>{period.authors}</td>
                        <td>{period.files}</td>
                        <td>{period.locAdded}</td>
                        <td>{period.locRemoved}</td>
                        <td>{period.loc}</td>
                    </tr>
                )}
            </tbody>
        </table>
    );
}

export default function Activity() {
    return (
        <Metric title="Activity" metric="Activity" renderData={renderActivity} />
    );
}
