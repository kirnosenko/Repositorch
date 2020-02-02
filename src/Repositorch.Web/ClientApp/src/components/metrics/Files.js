import React, { Fragment } from 'react';
import Metric from './Metric';
import SortableTable from '../table/SortableTable';

function renderData(data) {

	const columnsExts = [
		{ header: 'Extension', key: 'name' },
		{ header: 'Number of files (%)', key: 'files' },
		{ header: 'Added LOC', key: 'locAdded' },
		{ header: 'Removed LOC', key: 'locRemoved' },
		{ header: 'Remain LOC', key: 'locRemain' }
	];

	const columnsDirs = [
		{ header: 'Directory', key: 'name' },
		{ header: 'Number of files (%)', key: 'files' },
		{ header: 'Added LOC', key: 'locAdded' },
		{ header: 'Removed LOC', key: 'locRemoved' },
		{ header: 'Remain LOC', key: 'locRemain' }
	];

	return (
		<Fragment>
			<SortableTable
				data={data.exts}
				columns={columnsExts}
				className="table table-striped table-sm" />
			<SortableTable
				data={data.dirs}
				columns={columnsDirs}
				className="table table-striped table-sm" />
		</Fragment>
	);
}

export default function Files(props) {
	return (
		<Metric
			title="Files"
			path={`${props.project}/Files`}
			renderData={renderData} />
	);
}
