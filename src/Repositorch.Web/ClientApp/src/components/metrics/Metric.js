import React, { Fragment } from 'react';
import propTypes from 'prop-types';
import ContentToLoad from '../ContentToLoad';

export default function Metric(props) {

	const metricUrl = `api/Metrics/${props.projectMetricPath}`;

	function getData() {
		var data = props.getData();
		if (data === null || data.result === null) {
			return null;
		}
		return data;
	}

	function metricLayout(title, time, body) {
		return (
			<Fragment>
				<p>
					<b>{title}</b>
					&nbsp;
					<small>(generation time: {time})</small>
				</p>
				{body}
			</Fragment>
		);
	}

	async function loadMetric() {
		var url = metricUrl;
		var data = props.getData();
		var settings = data !== null ? data.settingsDelta : null;
		if (settings !== null) {
			var keys = Object.keys(settings);
			if (keys.length > 0) {
				var params = keys
					.map(k => k + '=' + encodeURIComponent(settings[k]))
					.join('&');
				url = url.concat('?', params)
			}
		}
		var response = await fetch(url);
		if (!response.ok) throw new Error(response.status);
		var json = await response.json();
		json["time"] = response.headers.get("Time");
		props.setData(json);
	}

	return (
		<ContentToLoad
			getData={getData}
			loadData={loadMetric}
			renderData={(metric) => metricLayout(
				props.title,
				metric.time,
				props.renderMetric(metric))} />
	);
}

Metric.propTypes = {
	title: propTypes.string.isRequired,
	projectMetricPath: propTypes.string.isRequired,
	renderMetric: propTypes.func.isRequired,
	getData: propTypes.func.isRequired,
	setData: propTypes.func.isRequired
};
