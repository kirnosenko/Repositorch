import React from 'react';
import Metric from './Metric';

function renderSummary(data) {
    return (
        <div>
            Statistics period: from {data.periodFrom} to {data.periodTo} ({data.periodDays} days, {data.periodYears} years)
            <br />Number of authors: {data.authors}
            <br />Number of commits: {data.commits}
            <br />Number of fix commits: {data.commitsFix} ({data.commitsFixPercent} %)
            <br />Number of refactoring commits: {data.commitsRefactoring} ({data.commitsRefactoringPercent} %)
            <br />Number of files: {data.files} ({data.filesAdded} added, {data.filesRemoved} removed)
            <br />Number of LOC: {data.loc} ({data.locAdded} added, {data.locRemoved} removed)
        </div>
    );
}

export default function Summary() {
    return (
        <Metric title="Summary" metric="Summary" renderData={renderSummary} />
    );
}
