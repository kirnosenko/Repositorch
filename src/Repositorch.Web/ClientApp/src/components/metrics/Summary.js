import React, { Fragment } from 'react';
import MetricStatic from './MetricStatic';

function renderResult(result) {
	return (
		<Fragment>
			Statistics period: from {result.periodFrom} to {result.periodTo} ({result.periodDays} days, {result.periodYears} years)
			<br />Number of authors: {result.authors}
			<br />Number of commits: {result.commits}
			<br />Number of fix commits: {result.commitsFix} ({result.commitsFixPercent} %)
			<br />Number of refactoring commits: {result.commitsRefactoring} ({result.commitsRefactoringPercent} %)
			<br />Number of files: {result.files} ({result.filesAdded} added, {result.filesRemoved} removed)
			<br />Number of LOC: {result.loc} ({result.locAdded} added, {result.locRemoved} removed)
		</Fragment>
	);
}

export default function Summary(props) {
	return (
		<MetricStatic
			title="Summary"
			projectMetricPath={props.projectMetricPath}
			renderResult={renderResult} />
	);
}
