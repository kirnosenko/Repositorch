import React from 'react';
import propTypes from 'prop-types';
import Metric from './Metric';

export default function MetricStatic(props) {

	const [data, setData] = React.useState(null);

	return (
		<Metric
			title={props.title}
			projectMetricPath={props.projectMetricPath}
			renderMetric={metric => props.renderResult(metric.result)}
			getData={() => data}
			setData={setData} />
	);
}

MetricStatic.propTypes = {
	title: propTypes.string.isRequired,
	projectMetricPath: propTypes.string.isRequired,
	renderResult: propTypes.func.isRequired
};
