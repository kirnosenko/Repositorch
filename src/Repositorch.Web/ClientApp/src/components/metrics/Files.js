import React from 'react';
import Metric from './Metric';
import SortableTable from '../table/sortable-table';

function renderFiles(data) {

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
        <div>
            <SortableTable
                data={data.exts}
                columns={columnsExts}
                className="table table-striped table-sm" />
            <SortableTable
                data={data.dirs}
                columns={columnsDirs}
                className="table table-striped table-sm" />
        </div>
    );
}

export default function Files() {
    return (
        <Metric title="Files" metric="Files" renderData={renderFiles} />
    );
}
