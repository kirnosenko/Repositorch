import React from 'react';
import Metric from './Metric';
import SortableTable from '../table/SortableTable';

function renderAuthors(data) {

    const columns = [
        { header: 'Author', key: 'name' },
        { header: 'Commits (%)', key: 'commits' },
        { header: 'Fix commits (%)', key: 'fixes' },
        { header: 'Refactoring commits (%)', key: 'refactorings' },
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
            data={data}
            columns={columns}
            className="table table-striped table-sm" />
    );
}

export default function Authors() {
    return (
        <Metric title="Authors" metric="Authors" renderData={renderAuthors} />
    );
}
