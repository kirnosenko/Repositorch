import React, { Fragment } from 'react';
import MetricStatic from './MetricStatic';
import SortableTable from '../table/SortableTable';

function renderResult(result) {

	const columnsExts = [
		{ header: 'Extension', key: 'name' },
		{ header: 'Number of files (%)', key: 'files' },
		{ header: 'Defects per KLOC', key: 'dd' },
		{ header: 'Added LOC', key: 'locAdded' },
		{ header: 'Removed LOC', key: 'locRemoved' },
		{ header: 'Remain LOC', key: 'locRemain' }
	];

	const columnsDirs = [
		{ header: 'Directory', key: 'name' },
		{ header: 'Number of files (%)', key: 'files' },
		{ header: 'Defects per KLOC', key: 'dd' },
		{ header: 'Added LOC', key: 'locAdded' },
		{ header: 'Removed LOC', key: 'locRemoved' },
		{ header: 'Remain LOC', key: 'locRemain' }
	];

	return (
		<Fragment>
			<SortableTable
				data={result.exts}
				columns={columnsExts}
				className="table table-striped table-sm" />
			<SortableTable
				data={result.dirs}
				columns={columnsDirs}
				className="table table-striped table-sm" />
		</Fragment>
	);
}

export default function Files(props) {
	return (
		<MetricStatic
			title="Files"
			projectMetricPath={props.projectMetricPath}
			renderResult={renderResult} />
	);
}
