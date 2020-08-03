import React, { Fragment } from 'react';
import {
	AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import { secondsToDateFormat, updateObject } from '../../functions';
import { getColors } from '../functions';
import Metric from '../../Metric';
import OwnershipForm from './OwnershipForm';

export default function Ownership(props) {

	const [data, setData] = React.useState(null);
	const settingsIn = ["path", "minimalContribution"];

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
				<Area
					type="monotone"
					name={title}
					key={dataKey}
					dataKey={dataKey}
					stackId="1"
					stroke={color}
					fill={color} />
			);
		}

		var colors = getColors(data.result.keys.length);

		return (
			<Fragment>
				<OwnershipForm
					settings={data.settings}
					updateSettings={updateSettings} />
				<ResponsiveContainer aspect={2} >
					<AreaChart data={data.result.values}>
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
						{data.result.keys.map((key, index) => {
							return LocLine(key, key, colors[index]);
						})}
					</AreaChart>
				</ResponsiveContainer>
			</Fragment>
		);
	}

	return (
		<Metric
			title="Lines Of Code Ownership"
			projectMetricPath={props.projectMetricPath}
			renderMetric={renderMetric}
			getData={() => data}
			setData={updateData} />
	);
}
