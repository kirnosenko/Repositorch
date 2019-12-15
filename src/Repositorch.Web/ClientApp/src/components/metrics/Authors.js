import React from 'react';
import { Metric } from './Metric';

function renderAuthors(data) {
    return (
        <table className='table table-striped' aria-labelledby="tabelLabel">
            <thead>
                <tr>
                    <th>Author</th>
                    <th>Commits (%)</th>
                    <th>Fix commits (%)</th>
                    <th>Refactoring commits (%)</th>
                    <th>Added LOC</th>
                    <th>Removed LOC</th>
                    <th>Remain LOC</th>
                    <th>Contribution %</th>
                    <th>Specialization %</th>
                    <th>Unique specialization %</th>
                    <th>Demand for code %</th>
                </tr>
            </thead>
            <tbody>
                {data.authors.map(author =>
                    <tr key={author.name}>
                        <td>{author.name}</td>
                        <td>{author.commits}</td>
                        <td>{author.fixes}</td>
                        <td>{author.refactorings}</td>
                        <td>{author.addedLoc}</td>
                        <td>{author.removedLoc}</td>
                        <td>{author.remainLoc}</td>
                        <td>{author.contribution}</td>
                        <td>{author.specialization}</td>
                        <td>{author.uniqueSpecialization}</td>
                        <td>{author.demandForCode}</td>
                    </tr>
                )}
            </tbody>
        </table>
    );
}

export default function Authors() {
    return (
        <Metric title="Authors" metric="Authors" renderData={renderAuthors} />
    );
}
