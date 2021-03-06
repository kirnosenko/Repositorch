﻿import React from 'react';
import MetricStatic from './MetricStatic';
import SortableTable from '../table/SortableTable';

function renderResult(result) {

	const columns = [
		{ header: 'Author', key: 'name' },
		{ header: 'Commits (%)', key: 'commits' },
		{ header: 'Fix commits (%)', key: 'fixes' },
		{ header: 'Refactoring commits (%)', key: 'refactorings' },
		{ header: 'Defects per KLOC', key: 'dd' },
		{ header: 'Added LOC', key: 'locAdded' },
		{ header: 'Removed LOC', key: 'locRemoved' },
		{ header: 'Remain LOC', key: 'locRemain' },
		{ header: 'Contribution %', key: 'contribution' },
		{ header: 'Specialization %', key: 'specialization' },
		{ header: 'Unique specialization %', key: 'uniqueSpecialization' },
		{ header: 'Demand for code %', key: 'demandForCode' },
	];

	return (
		<SortableTable
			data={result}
			columns={columns}
			className="table table-striped table-sm" />
	);
}

export default function Authors(props) {
	return (
		<MetricStatic
			title="Authors"
			projectMetricPath={props.projectMetricPath}
			renderResult={renderResult} />
	);
}
