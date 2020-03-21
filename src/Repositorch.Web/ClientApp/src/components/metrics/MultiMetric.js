import React, { Fragment } from 'react';
import propTypes from 'prop-types';
import ContentToLoad from '../ContentToLoad';

export default function Metric(props) {

	const settingsUrl = `api/Metrics/GetSettings/${props.projectMetricPath}`;
	const metricUrl = `api/Metrics/Calculate/${props.projectMetricPath}`;

	async function loadMetric() {
		var url = settingsUrl;
		var settings = props.getSettings();
		if (settings !== null) {
			url = metricUrl;
			var keys = Object.keys(settings).filter(k => {
				var value = settings[k];
				return value !== null && value !== '' && value !== false && value !== 0
			});
			if (keys.length > 0) {
				var params = keys
					.map(k =>
						encodeURIComponent(k) + '=' + encodeURIComponent(settings[k]))
					.join('&');
				url = url.concat('?', params)
			}
		}
		var response = await fetch(url);
		if (!response.ok) throw new Error(response.status);
		var json = await response.json();
		if (settings === null) {
			props.setFormData(json);
		}
		else {
			props.setData(json);
		}
	}

	function getMetricTime(metric) {
		if (metric.time !== undefined) {
			if (metric.time === null) {
				return 'from cache';
			}
			else {
				return metric.time;
			}
		}

		return 'unknown';
	}

	function renderMetric(metric) {
		return (
			<Fragment>
				<p>
					<b>{props.title}</b>
					&nbsp;
					<small>(generation time: {getMetricTime(metric)})</small>
				</p>
				{props.renderData(metric.data)}
			</Fragment>
		);
	}

	return (
		<ContentToLoad
			getData={props.getData}
			loadData={loadMetric}
			renderData={renderMetric} />
	);
}

Metric.propTypes = {
	title: propTypes.string.isRequired,
	projectMetricPath: propTypes.string.isRequired,
	renderData: propTypes.func.isRequired,
	getData: propTypes.func.isRequired,
	setData: propTypes.func.isRequired,
	getSettings: propTypes.func.isRequired,
	setFormData: propTypes.func.isRequired
};
