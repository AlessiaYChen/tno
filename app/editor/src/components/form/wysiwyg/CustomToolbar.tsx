import { FaCode, FaExpandAlt, FaPaintBrush, FaRemoveFormat } from 'react-icons/fa';
import { TiDelete } from 'react-icons/ti';
export interface ICustomToolbarProps {
  onClickRaw: React.MouseEventHandler<HTMLButtonElement>;
  onClickRemoveFormat: React.MouseEventHandler<HTMLButtonElement>;
  onClickFormatRaw: React.MouseEventHandler<HTMLButtonElement>;
  onClickClear: React.MouseEventHandler<HTMLButtonElement>;
  innerRef: any;
}

/** Custom toolbar for the content WYSIWYG including ability to view HTML source and clear formatting */
export const CustomToolbar: React.FC<ICustomToolbarProps> = ({
  onClickRaw,
  onClickRemoveFormat,
  onClickFormatRaw,
  onClickClear,
  innerRef,
}) => (
  <div ref={innerRef} className="toolbar">
    <span className="ql-formats">
      <select className="ql-header" />
      <button className="ql-bold" />
      <button className="ql-italic" />
      <button className="ql-underline" />
      <button className="ql-strike" />
    </span>
    <span className="ql-formats">
      <select className="ql-align" />
      <select className="ql-color" />
    </span>
    <span className="ql-formats">
      <button className="ql-list" value="ordered" />
      <button className="ql-list" value="bullet" />
      <button className="ql-indent" value="-1" />
      <button className="ql-indent" value="+1" />
    </span>
    <span className="ql-formats">
      <button type="button" onClick={onClickRaw}>
        <FaCode className="custom-icon" />
      </button>
      <button type="button" onClick={onClickRemoveFormat}>
        <FaRemoveFormat className="custom-icon" />
      </button>
      <button type="button" onClick={onClickFormatRaw}>
        <FaPaintBrush className="custom-icon" />
      </button>
      <button className="ql-link"></button>
      <button className="ql-image"></button>
    </span>
    <span className="ql-formats">
      <button>
        <FaExpandAlt className="custom-icon" />
      </button>
      <button type="button" onClick={onClickClear}>
        <TiDelete className="custom-icon delete" />
      </button>
    </span>
  </div>
);
