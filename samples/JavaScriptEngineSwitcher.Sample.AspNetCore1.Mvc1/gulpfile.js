/*global require, exports */
/*jshint esversion: 6 */

// include plug-ins
const { src, dest, series, parallel, watch } = require('gulp');
const del = require('del');
const sourcemaps = require('gulp-sourcemaps');
const rename = require('gulp-rename');
const concat = require('gulp-concat');
const less = require('gulp-less');
const autoprefixer = require('gulp-autoprefixer');
const cleanCss = require('gulp-clean-css');
const uglify = require('gulp-uglify');

const webRootPath = "wwwroot";
const bowerDirPath = webRootPath + "/lib";
const styleDirPath = webRootPath + '/styles';
const scriptDirPath = webRootPath + '/scripts';

//#region Clean
//#region Clean builded assets
function cleanBuildedStyles() {
	return del([styleDirPath + '/build/*']);
}

function cleanBuildedScripts() {
	return del([scriptDirPath + '/build/*']);
}

const cleanBuildedAssets = parallel(cleanBuildedStyles, cleanBuildedScripts);
//#endregion
//#endregion

//#region Build assets
//#region Build styles
const autoprefixerOptions = {
	overrideBrowserslist: ['> 1%', 'last 3 versions', 'Firefox ESR', 'Opera 12.1'],
	cascade: true
};
const cssCleanOptions = { specialComments: '*' };
const cssRenameOptions = { extname: '.min.css' };

function buildCommonStyles() {
	return src([styleDirPath + '/app.less'])
		.pipe(sourcemaps.init())
		.pipe(less({
			relativeUrls: true,
			rootpath: '/styles/'
		}))
		.pipe(autoprefixer(autoprefixerOptions))
		.pipe(sourcemaps.write('./'))
		.pipe(dest(styleDirPath + '/build'))
		.pipe(sourcemaps.init({ loadMaps: true }))
		.pipe(concat('common-styles.css'))
		.pipe(cleanCss(cssCleanOptions))
		.pipe(rename(cssRenameOptions))
		.pipe(sourcemaps.write('./'))
		.pipe(dest(styleDirPath + '/build'))
		;
}

const buildStyles = buildCommonStyles;
//#endregion

//#region Build scripts
const jsConcatOptions = { newLine: ';' };
const jsUglifyOptions = {
	output: { comments: /^!/ }
};
const jsRenameOptions = { extname: '.min.js' };

function buildModernizrScripts() {
	return src([bowerDirPath + '/modernizr/modernizr.js'])
		.pipe(sourcemaps.init())
		.pipe(uglify(jsUglifyOptions))
		.pipe(rename(jsRenameOptions))
		.pipe(sourcemaps.write('./'))
		.pipe(dest(scriptDirPath + '/build'))
		;
}

function buildCommonScripts() {
	return src([scriptDirPath + '/common.js'])
		.pipe(sourcemaps.init({ loadMaps: true }))
		.pipe(rename({ basename: 'common-scripts' }))
		.pipe(uglify(jsUglifyOptions))
		.pipe(rename(jsRenameOptions))
		.pipe(sourcemaps.write('./'))
		.pipe(dest(scriptDirPath + '/build'))
		;
}

function buildEvaluationFormScripts() {
	return src([bowerDirPath + '/jquery-validation/dist/jquery.validate.js',
			bowerDirPath + '/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js',
			bowerDirPath + '/bootstrap/js/button.js',
			scriptDirPath + '/evaluation-form.js'])
		.pipe(sourcemaps.init({ loadMaps: true }))
		.pipe(concat('evaluation-form-scripts.js', jsConcatOptions))
		.pipe(uglify(jsUglifyOptions))
		.pipe(rename(jsRenameOptions))
		.pipe(sourcemaps.write('./'))
		.pipe(dest(scriptDirPath + '/build'))
		;
}

const buildScripts = parallel(buildModernizrScripts, buildCommonScripts, buildEvaluationFormScripts);
//#endregion

const buildAssets = parallel(buildStyles, buildScripts);
//#endregion

//#region Watch assets
function watchStyles() {
	return watch([styleDirPath + '/**/*.{less,css}', '!' + styleDirPath + '/build/**/*.*'],
		buildStyles);
}

function watchScripts() {
	return watch([scriptDirPath + '/**/*.js', '!' + scriptDirPath + '/build/**/*.*'],
		buildScripts);
}

const watchAssets = parallel(watchStyles, watchScripts);
//#endregion

// Export tasks
exports.cleanBuildedAssets = cleanBuildedAssets;
exports.buildAssets = buildAssets;
exports.watchAssets = watchAssets;
exports.default = series(cleanBuildedAssets, buildAssets);