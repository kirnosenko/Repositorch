import React, { Fragment } from 'react';
import Metric from '../../Metric';

function renderMetricData(data) {
	return (
		<Fragment>
			Number of commits: {data}
		</Fragment>
	);
}

export default function Subsubmetric(props) {
	return (
		<Metric
			title="Submetric"
			path={`${props.project}/${Subsubmetric.name}`}
			renderData={renderMetricData} />
	);
}
