﻿import React from 'react';
import MetricStatic from './MetricStatic';
import SortableTable from '../table/SortableTable';

function renderResult(result) {

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
		<SortableTable
			data={result}
			columns={columns}
			className="table table-striped table-sm" />
	);
}

export default function Activity(props) {
	return (
		<MetricStatic
			title="Activity"
			projectMetricPath={props.projectMetricPath}
			renderResult={renderResult} />
	);
}
