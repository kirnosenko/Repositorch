import React, { Fragment } from 'react';
import {
	LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import { secondsToDateFormat, updateObject } from '../../functions';
import { getColors } from '../functions';
import Metric from '../../Metric';
import SummaryForm from './SummaryForm';

export default function Summary(props) {

	const [data, setData] = React.useState(null);
	const settingsIn = ["author", "path"];

	function updateSettings(settings) {
		var settingsDelta = {};
		Object.keys(data.settings).forEach(key => {
			if (data.settings[key] !== settings[key]) {
				settingsDelta[key] = settings[key];
			}
		});
		var newData = {
			settings: settingsDelta,
			settingsDelta: settingsDelta
		};
		var needUpdate = Object.keys(settingsDelta).some(s => {
			return settingsIn.includes(s);
		});
		if (needUpdate) {
			newData.result = null;
		}
		updateData(newData);
	}

	function updateData(data) {
		setData((prevState, props) => {
			if (prevState === null) {
				return data;
			}
			return updateObject({ ...prevState }, data);
		});
	}

	function renderMetric(data) {

		function LocLine(title, dataKey, color) {
			return (
				data.settings[dataKey] ?
					<Line
						type="monotone"
						name={title}
						dataKey={dataKey}
						stroke={color}
						dot={{ r: 0 }} /> : ""
			);
		}

		var colors = getColors(3);
		
		return (
			<Fragment>
				<SummaryForm
					settings={data.settings}
					updateSettings={updateSettings} />
				<ResponsiveContainer aspect={2} >
					<LineChart data={data.result}>
						<CartesianGrid strokeDasharray="3 3" />
						<XAxis
							dataKey="date"
							type="number"
							tickFormatter={secondsToDateFormat}
							domain={[data.settings["dateFrom"], data.settings["dateTo"]]}
							allowDataOverflow />
						<YAxis />
						<Tooltip labelFormatter={secondsToDateFormat} />
						<Legend />
						{LocLine("LOC total", "locTotal", colors[0])}
						{LocLine("LOC added", "locAdded", colors[1])}
						{LocLine("LOC removed", "locRemoved", colors[2])}
					</LineChart>
				</ResponsiveContainer>
			</Fragment>
		);
	}

	return (
		<Metric
			title="Lines Of Code Summary"
			projectMetricPath={props.projectMetricPath}
			renderMetric={renderMetric}
			getData={() => data}
			setData={updateData} />
	);
}
