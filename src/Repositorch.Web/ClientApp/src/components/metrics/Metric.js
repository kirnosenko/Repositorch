import React, { Fragment } from 'react';
import propTypes from 'prop-types';
import ContentToLoadClosed from '../ContentToLoadClosed';

export default function Metric(props) {

	function renderMetric(metric) {

		var time = 'unknown';
		if (metric.time !== undefined) {
			if (metric.time === null) {
				time = 'from cache';
			}
			else {
				time = metric.time;
			}
		}

		return (
			<Fragment>
				<p>
					<b>{props.title}</b>
					&nbsp;
					<small>(generation time: {time})</small>
				</p>
				{props.renderResult(metric.result)}
			</Fragment>
		);
	}

	return (
		<ContentToLoadClosed
			url={`api/Metrics/${props.projectMetricPath}`}
			renderData={renderMetric} />
	);
}

Metric.propTypes = {
	title: propTypes.string.isRequired,
	projectMetricPath: propTypes.string.isRequired,
	renderResult: propTypes.func.isRequired
};
