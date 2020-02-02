import React, { Fragment } from 'react';
import Metric from './Metric';

function renderData(data) {
	return (
		<Fragment>
			Statistics period: from {data.periodFrom} to {data.periodTo} ({data.periodDays} days, {data.periodYears} years)
			<br />Number of authors: {data.authors}
			<br />Number of commits: {data.commits}
			<br />Number of fix commits: {data.commitsFix} ({data.commitsFixPercent} %)
			<br />Number of refactoring commits: {data.commitsRefactoring} ({data.commitsRefactoringPercent} %)
			<br />Number of files: {data.files} ({data.filesAdded} added, {data.filesRemoved} removed)
			<br />Number of LOC: {data.loc} ({data.locAdded} added, {data.locRemoved} removed)
		</Fragment>
	);
}

export default function Summary(props) {
	return (
		<Metric
			title="Summary"
			path={`${props.project}/Summary`}
			renderData={renderData} />
	);
}
