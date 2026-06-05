import { render, screen, fireEvent } from '@testing-library/react';
import { TodoFilter } from './TodoFilter';

describe('TodoFilter', () => {
    it('renders All, Pending, and Completed buttons', () => {
        render(<TodoFilter onFilter={vi.fn()} />);
        expect(screen.getByRole('button', { name: 'All' })).toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Pending' })).toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Completed' })).toBeInTheDocument();
    });

    it('calls onFilter with "Pending" when Pending is clicked', () => {
        const onFilter = vi.fn();
        render(<TodoFilter onFilter={onFilter} />);
        fireEvent.click(screen.getByRole('button', { name: 'Pending' }));
        expect(onFilter).toHaveBeenCalledWith('Pending');
    });

    it('calls onFilter with "All" when All is clicked', () => {
        const onFilter = vi.fn();
        render(<TodoFilter onFilter={onFilter} />);
        fireEvent.click(screen.getByRole('button', { name: 'All' }));
        expect(onFilter).toHaveBeenCalledWith('All');
    });
});
