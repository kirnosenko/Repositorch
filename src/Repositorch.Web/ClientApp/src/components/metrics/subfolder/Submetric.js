import React, { Fragment } from 'react';
import Metric from '../Metric';

function renderMetricData(data) {
	return (
		<Fragment>
			Number of commits: {data}
		</Fragment>
	);
}

export default function Submetric(props) {
	return (
		<Metric
			title="Submetric"
			projectMetricPath={props.projectMetricPath}
			renderData={renderMetricData} />
	);
}
