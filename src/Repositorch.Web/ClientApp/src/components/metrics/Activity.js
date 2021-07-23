import React, { Fragment } from 'react';
import { updateObject } from './functions';
import Metric from './Metric';
import ActivityForm from './ActivityForm';
import SortableTable from '../table/SortableTable';

export default function Activity(props) {

	const [data, setData] = React.useState(null);
	const settingsIn = ["slice", "path"];

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

		const columns = [
			{ header: 'Period', key: 'period' },
			{ header: 'Commits (total)', key: 'commits' },
			{ header: 'Authors (total)', key: 'authors' },
			{ header: 'Files', key: 'files' },
			{ header: 'Defects fixed (total)', key: 'defectsFixed' },
			{ header: 'Added LOC (total)', key: 'locAdded' },
			{ header: 'Removed LOC (total)', key: 'locRemoved' },
			{ header: 'Remain LOC', key: 'locRemain' }
		];

		return (
			<Fragment>
				<ActivityForm
					settings={data.settings}
					updateSettings={updateSettings} />
				<SortableTable
					data={data.result}
					columns={columns}
					className="table table-striped table-sm" />
			</Fragment>
		);
	}

	return (
		<Metric
			title="Activity"
			projectMetricPath={props.projectMetricPath}
			renderMetric={renderMetric}
			getData={() => data}
			setData={updateData} />
	);
}
