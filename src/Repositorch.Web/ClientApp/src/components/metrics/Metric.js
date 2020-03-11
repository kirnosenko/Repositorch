import React, { Fragment } from 'react';
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
				{props.renderData(metric.data)}
			</Fragment>
		);
	}

	return (
		<ContentToLoadClosed
			url={`api/Metrics/${props.projectMetricPath}`}
			renderData={renderMetric} />
	);
}
