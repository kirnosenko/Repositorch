import React from 'react';
import propTypes from 'prop-types';
import { metricLayout } from './functions';
import ContentToLoad from '../ContentToLoad';

export default function MultiMetric(props) {

	const metricUrl = `api/Metrics/${props.projectMetricPath}`;

	function getData() {
		var data = props.getData();
		if (data === null || data.result === null) {
			return null;
		}
		return data;
	}

	async function loadMetric() {
		var url = metricUrl;
		var data = props.getData();
		var settings = data !== null ? data.settings : null;
		if (settings !== null) {
			var keys = Object.keys(settings).filter(k => {
				var value = settings[k];
				return value !== null && value !== '' && value !== false && value !== 0
			});
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

MultiMetric.propTypes = {
	title: propTypes.string.isRequired,
	projectMetricPath: propTypes.string.isRequired,
	renderMetric: propTypes.func.isRequired,
	getData: propTypes.func.isRequired,
	setData: propTypes.func.isRequired
};
