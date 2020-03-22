import React from 'react';
import propTypes from 'prop-types';
import { metricLayout } from './functions';
import ContentToLoadClosed from '../ContentToLoadClosed';

export default function Metric(props) {
	return (
		<ContentToLoadClosed
			url={`api/Metrics/${props.projectMetricPath}`}
			renderData={(metric) => metricLayout(
				props.title,
				metric.time,
				props.renderResult(metric.result))} />
	);
}

Metric.propTypes = {
	title: propTypes.string.isRequired,
	projectMetricPath: propTypes.string.isRequired,
	renderResult: propTypes.func.isRequired
};
