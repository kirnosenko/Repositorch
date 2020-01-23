﻿import React from 'react';
import Metric from './Metric';
import SortableTable from '../table/sortable-table';

function renderActivity(data) {

    const columns = [
        { header: 'Period', key: 'period' },
        { header: 'Commits (total)', key: 'commits' },
        { header: 'Authors (total)', key: 'authors' },
        { header: 'Files', key: 'files' },
        { header: 'Added LOC (total)', key: 'locAdded' },
        { header: 'Removed LOC (total)', key: 'locRemoved' },
        { header: 'Remain LOC', key: 'locRemain' }
    ];

    return (
        <SortableTable
            data={data}
            columns={columns}
            className="table table-striped table-sm" />
    );
}

export default function Activity() {
    return (
        <Metric title="Activity" metric="Activity" renderData={renderActivity} />
    );
}